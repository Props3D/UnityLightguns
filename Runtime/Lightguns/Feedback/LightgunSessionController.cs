using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Blamcon.Lightguns
{
    /// <summary>
    /// The logic behind <see cref="LightgunSession"/>, kept out of the MonoBehaviour so it can be tested
    /// against a chosen set of devices.
    /// </summary>
    internal sealed class LightgunSessionController
    {
        readonly Func<IEnumerable<InputDevice>> m_Devices;

        public bool recoil = true;
        public bool rumble = true;
        public bool led = true;
        public bool ammo;

        /// <summary>Between <see cref="Begin"/> and <see cref="End"/>.</summary>
        public bool active { get; private set; }

        /// <summary>Control handed back by <see cref="Pause"/> until <see cref="Resume"/>.</summary>
        public bool paused { get; private set; }

        /// <param name="devices">The devices to look for guns in; the Input System's device list in a game.</param>
        public LightgunSessionController(Func<IEnumerable<InputDevice>> devices)
        {
            m_Devices = devices ?? throw new ArgumentNullException(nameof(devices));
        }

        /// <summary>Takes control of the chosen components on every player's gun.</summary>
        public void Begin()
        {
            if (active)
                return;
            active = true;
            paused = false;
            SendToEveryPlayer(take: true);
        }

        /// <summary>Hands the chosen components back on every player's gun.</summary>
        public void End()
        {
            if (!active)
                return;
            active = false;
            if (!paused)
                SendToEveryPlayer(take: false);
            paused = false;
        }

        /// <summary>Hands control back until <see cref="Resume"/>, e.g. while the application lacks focus.</summary>
        public void Pause()
        {
            if (!active || paused)
                return;
            paused = true;
            SendToEveryPlayer(take: false);
        }

        /// <summary>Takes control again after <see cref="Pause"/>.</summary>
        public void Resume()
        {
            if (!active || !paused)
                return;
            paused = false;
            SendToEveryPlayer(take: true);
        }

        /// <summary>Takes control of a gun that connected or reconnected during the session.</summary>
        public void DeviceAdded(InputDevice device)
        {
            if (!active || paused)
                return;
            if (!(device is BlamconLightgunHID) && !(device is BlamconMouseModeDevice))
                return;
            var player = BlamconDevices.PlayerIndexFrom(device.description);
            if (player >= 0)
                SendToPlayer(player, take: true);
        }

        void SendToEveryPlayer(bool take)
        {
            for (var player = 0; player < BlamconDevices.kPlayerCount; player++)
                SendToPlayer(player, take);
        }

        void SendToPlayer(int player, bool take)
        {
            if (!recoil && !rumble && !led && !ammo)
                return;
            var device = BlamconDevices.SelectFeedbackDevice(m_Devices(), player);
            if (device == null)
                return;
            var report = BlamconForceFeedback.ControlReport(recoil, rumble, led, ammo, take);
            BlamconForceFeedback.Send(device, ref report, take ? "take control command" : "release control command");
        }
    }
}
