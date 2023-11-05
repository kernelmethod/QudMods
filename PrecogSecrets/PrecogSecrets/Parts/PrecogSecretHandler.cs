using Qud.API;
using System;
using System.Collections.Generic;
using XRL;
using XRL.Messages;
using XRL.World;
using XRL.UI;

namespace Kernelmethod.PrecogSecrets {
    [Serializable]
    public class PrecogSecretHandler : IPart {
        public const string Prefix = "Kernelmethod_PrecogSecrets";

        public Dictionary<string, bool> KnownSecrets = new Dictionary<string, bool>();
        public bool Activated = false;

        public override bool WantEvent(int ID, int cascade) {
            return ID == AfterPlayerBodyChangeEvent.ID
                || ID == GetPrecognitionRestoreGameStateEvent.ID
                || ID == GenericDeepNotifyEvent.ID
                || base.WantEvent(ID, cascade);
        }

        public override void Register(GameObject obj) {
            obj.RegisterPartEvent(this, "InitiatePrecognition");
            obj.RegisterPartEvent(this, "BeforeSecretRevealed");
            obj.RegisterPartEvent(this, "BeforeSecretForgotten");
        }

        public override bool HandleEvent(AfterPlayerBodyChangeEvent E) {
            // Copy over state from old body to new body
            var handlerPart = new PrecogSecretHandler();
            handlerPart.KnownSecrets = this.KnownSecrets;
            handlerPart.Activated = this.Activated;

            E.NewBody.RemovePart("PrecogSecretHandler");
            E.NewBody.AddPart(handlerPart);

            if (E.OldBody != null)
                E.OldBody.RemovePart("PrecogSecretHandler");

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetPrecognitionRestoreGameStateEvent E) {
            LogInfo("saving secret state to game dictionary");

            // Register the secrets that have been learned or forgotten to the state dictionary
            // prior to reverting to the previous save.
            foreach (KeyValuePair<string, bool> secretState in KnownSecrets) {
                var gameStateKey = GetSecretTransferKey(secretState.Key);
                LogInfo($"saving state known = {secretState.Value} for secret {secretState.Key} in {gameStateKey}");
                E.Set(gameStateKey, secretState.Value);
            }

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GenericDeepNotifyEvent E) {
            if (E.Notify != "PrecognitionGameRestored")
                return base.HandleEvent(E);

            try {
                RecallSecrets();
            }
            catch (Exception ex) {
                LogException("error recalling secret state:", ex);
            }
            return base.HandleEvent(E);
        }

        public void RecallSecrets() {
            LogInfo("finding learned and forgotten secrets");

            // Construct a dictionary with secrets to remember/forget
            var secrets = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, bool> gameState in The.Game.BooleanGameState) {
                if (!gameState.Key.StartsWith(Prefix))
                    continue;

                var secretid = gameState.Key.Substring(Prefix.Length + 1);
                secrets[secretid] = gameState.Value;
                LogInfo($"extracted secret {secretid} from game state dictionary");
            }

            // Remove keys from the game's state dictionary
            foreach (KeyValuePair<string, bool> secret in secrets) {
                var key = GetSecretTransferKey(secret.Key);
                The.Game.RemoveBooleanGameState(key);
                LogInfo($"removed {key} from the game state dictionary");
            }

            LogInfo("restoring learned and forgotten secrets");

            // Search for journal entries with the IDs that were inserted into the dictionary, and
            // use them to remember or forget secrets.
            foreach (IBaseJournalEntry entry in JournalAPI.GetAllNotes()) {
                var secretid = entry.ID;
                if (secretid == null || !secrets.ContainsKey(secretid))
                    continue;

                var known = secrets[entry.ID];
                LogInfo($"restoring secret {secretid} (known = {known})");

                if (known)
                    entry.Reveal(Silent: true);
                else
                    entry.Forget();

                // Pop key from the dictionary
                secrets.Remove(entry.ID);

                // Exit if there are no more secrets left to process
                if (secrets.Count == 0)
                    break;
            }

            // Deactivate the part
            Activated = false;
        }

        public override bool FireEvent(Event E) {
            if (E.ID == "InitiatePrecognition")
                Activated = true;
            else if (E.ID == "BeforeSecretRevealed" && Activated) {
                // Register that the secret was revealed
                IBaseJournalEntry secret = E.GetParameter<IBaseJournalEntry>("Secret");
                if (secret.ID != null) {
                    KnownSecrets[secret.ID] = true;
                    LogInfo($"learned secret {secret.ID} during precognition");
                }
            }
            else if (E.ID == "BeforeSecretForgotten" && Activated) {
                // Register that the secret was forgotten
                IBaseJournalEntry secret = E.GetParameter<IBaseJournalEntry>("Secret");
                if (secret.ID != null) {
                    KnownSecrets[secret.ID] = false;
                    LogInfo($"forgot secret {secret.ID} during precognition");
                }
            }

            return base.FireEvent(E);
        }

        private string GetSecretTransferKey(string secretid) {
            return $"{Prefix}_{secretid}";
        }

        private static void LogInfo(string Message) {
            MetricsManager.LogInfo($"{Prefix}: {Message}");
        }

        private static void LogException(string Message, Exception ex) {
            MetricsManager.LogException($"{Prefix}: {Message}", ex);
        }
    }
}
