using Unity.Entities;

namespace CrowbaneArena.Models
{
    /// <summary>
    /// Represents player data including user and character entity references.
    /// </summary>
    public struct PlayerData
    {
        /// <summary>
        /// The user entity associated with this player.
        /// </summary>
        public Entity UserEntity { get; }
        
        /// <summary>
        /// The character entity controlled by this player.
        /// </summary>
        public Entity CharacterEntity { get; }
        
        /// <summary>
        /// The player's character name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The player's Steam ID.
        /// </summary>
        public ulong SteamId { get; }
        
        /// <summary>
        /// Indicates whether the player is currently connected.
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerData"/> struct.
        /// </summary>
        /// <param name="name">The player's character name.</param>
        /// <param name="steamId">The player's Steam ID.</param>
        /// <param name="isConnected">Whether the player is connected.</param>
        /// <param name="userEntity">The user entity.</param>
        /// <param name="characterEntity">The character entity.</param>
        public PlayerData(string name, ulong steamId, bool isConnected, Entity userEntity, Entity characterEntity)
        {
            Name = name;
            SteamId = steamId;
            IsConnected = isConnected;
            UserEntity = userEntity;
            CharacterEntity = characterEntity;
        }
    }
}
