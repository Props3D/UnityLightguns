namespace Blamcon.Lightguns
{
    /// <summary>How a gun presents itself to the system. Aim and buttons only reach a game in Gamepad mode.</summary>
    public enum LightgunMode
    {
        Unknown,
        Mouse,
        Gamepad,
    }

    /// <summary>The board in the gun, worked out from its firmware version.</summary>
    public enum LightgunBoard
    {
        Unknown,
        RP2040,
        RP2350,
    }

    /// <summary>
    /// What the package knows about one player's gun: use it to show hardware state in a settings screen,
    /// and to decide what to offer, since a gun in mouse mode has no gun input and older firmware takes
    /// feedback only in Gamepad mode.
    /// </summary>
    /// <remarks>
    /// Device facts only. Game preferences, such as the player's chosen LED colour or whether they want
    /// rumble, belong in the game's own settings.
    /// <para>
    /// The gun answers richer details in HID feature report <c>0x50</c>, but Unity's Input System has no
    /// feature-report command, so everything here comes from the device description and from which devices
    /// the Input System created.
    /// </para>
    /// </remarks>
    public struct BlamconLightgunInfo
    {
        /// <summary>A gun is connected for this player index. Every other field is default when false.</summary>
        public bool connected { get; internal set; }

        /// <summary>0-based, as passed to the other calls: player 1 is 0. -1 when no gun is connected.</summary>
        public int playerIndex { get; internal set; }

        /// <summary>The gun sends aim and buttons. False in mouse mode, where aim comes from Unity's mouse.</summary>
        public bool hasGunInput { get; internal set; }

        /// <summary>
        /// The firmware takes force feedback in this mode. It doesn't say whether a solenoid or motor is
        /// fitted, which only feature report <c>0x50</c> knows.
        /// </summary>
        public bool feedbackAvailable { get; internal set; }

        /// <summary>
        /// The gun reported a firmware version the package could read. When false, <see cref="firmwareVersion"/>
        /// is empty, <see cref="firmwareVersionNumber"/> is 0 and <see cref="board"/> is
        /// <see cref="LightgunBoard.Unknown"/>, while the gun still works.
        /// </summary>
        public bool detailsKnown { get; internal set; }

        /// <summary>"3.0.0", or empty when the version couldn't be read.</summary>
        public string firmwareVersion { get; internal set; }

        /// <summary>
        /// major * 10000 + minor * 100 + patch, so 3.0.0 is 30000 and versions compare with &gt;=. 0 when
        /// unknown. The same encoding the firmware reports in feature report <c>0x50</c> and the Unreal
        /// plugin exposes, so the two engines compare versions the same way.
        /// </summary>
        public int firmwareVersionNumber { get; internal set; }

        /// <summary>
        /// RP2040 below firmware 3.0.0, RP2350 from 3.0.0 on, which is the first firmware that shipped on
        /// that board. Unknown when the firmware version is unknown.
        /// </summary>
        public LightgunBoard board { get; internal set; }

        /// <summary>Gamepad or Mouse, from the device the gun is reached through.</summary>
        public LightgunMode mode { get; internal set; }

        /// <summary>
        /// The player number set on the gun itself, 1-4, or 0 when unknown. <see cref="playerIndex"/> is
        /// this minus one.
        /// </summary>
        public int playerNumberOnGun { get; internal set; }

        /// <summary>The gun's USB product name, for showing which device this is.</summary>
        public string productName { get; internal set; }
    }
}
