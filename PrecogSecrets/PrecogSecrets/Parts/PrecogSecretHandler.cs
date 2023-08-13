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
        public static string Prefix = "Kernelmethod_PrecogSecrets";

        public Dictionary<string, bool> RevealedSecrets = new Dictionary<string, bool>();
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
            E.NewBody.RequirePart<PrecogSecretHandler>();
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetPrecognitionRestoreGameStateEvent E) {
            MetricsManager.LogInfo($"{Prefix}: Saving secret state to state dictionary");

            // Register the secrets that have been learned or forgotten to the state dictionary
            // prior to reverting to the previous save.
            foreach (KeyValuePair<string, bool> secretState in RevealedSecrets) {
                var gameStateKey = GetSecretTransferKey(secretState.Key);
                MetricsManager.LogInfo($"{Prefix}: saving state known = {secretState.Value} for secret {secretState.Key} in {gameStateKey}");
                E.Set(gameStateKey, secretState.Value);
            }

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GenericDeepNotifyEvent E) {
            if (E.Notify != "PrecognitionGameRestored")
                return base.HandleEvent(E);

            MetricsManager.LogInfo($"{Prefix}: Restoring learned and forgotten secrets");

            // Restore learned secrets from the game's state dictionary
            foreach (KeyValuePair<string, bool> gameState in The.Game.BooleanGameState) {
                MetricsManager.LogInfo($"{Prefix}: Checking gameState.Key = {gameState.Key}");

                if (gameState.Key.StartsWith(Prefix)) {
                    var secretid = gameState.Key.Substring(Prefix.Length + 1);
                    RestoreSecretState(secretid, gameState.Value);
                    The.Game.RemoveBooleanGameState(gameState.Key);
                }
            }

            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E) {
            if (E.ID == "InitiatePrecognition")
                Activated = true;
            else if (E.ID == "BeforeSecretRevealed" && Activated) {
                // Register that the secret was revealed
                IBaseJournalEntry secret = E.GetParameter<IBaseJournalEntry>("Secret");
                RevealedSecrets[secret.secretid] = true;
                MetricsManager.LogInfo($"{Prefix}: learned secret {secret.secretid} during precognition");
            }
            else if (E.ID == "BeforeSecretForgotten" && Activated) {
                // Register that the secret was forgotten
                IBaseJournalEntry secret = E.GetParameter<IBaseJournalEntry>("Secret");
                RevealedSecrets[secret.secretid] = false;
                MetricsManager.LogInfo($"{Prefix}: forgot secret {secret.secretid} during precognition");
            }

            return base.FireEvent(E);
        }

        private void RestoreSecretState(string secretid, bool known) {
            MetricsManager.LogInfo($"{Prefix}: Restoring secret {secretid} (known = {known})");

            // Find the note with the given ID and reveal or forget it
            foreach (IBaseJournalEntry entry in JournalAPI.GetAllNotes()) {
                if (entry.secretid == secretid) {
                    if (known)
                        entry.Reveal(silent: true);
                    else
                        entry.Forget(fast: true);
                    break;
                }
            }
        }

        private string GetSecretTransferKey(string secretid) {
            return $"{Prefix}_{secretid}";
        }
    }
}
