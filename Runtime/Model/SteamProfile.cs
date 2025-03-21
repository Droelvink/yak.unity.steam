using Steamworks;
using UnityEngine;

namespace com.yak.steam
{
    public class SteamProfile
    {
        private Texture2D _cachedAvatar;
        
        public CSteamID Id { get; }
        public ulong Steam64Id { get; }
        public string Name { get; }
        public string Nickname { get; }
        public AccountID_t AccountId { get; }
        public bool IsSelf => Id == SteamUser.GetSteamID();
        public bool IsFriend => SteamFriends.HasFriend(Id, EFriendFlags.k_EFriendFlagImmediate);
        public EPersonaState State { get; private set; }

        public SteamProfile(CSteamID steamID)
        {
            Id = steamID;
            Name = SteamFriends.GetFriendPersonaName(steamID);
            Nickname = SteamFriends.GetPlayerNickname(steamID);
            AccountId = Id.GetAccountID();
            Steam64Id = Id.m_SteamID;
            State = SteamFriends.GetFriendPersonaState(Id);
        }

        public async Awaitable<Texture2D> GetAvatarAsync(SteamAvatarSize avatarSize = SteamAvatarSize.Large) => await GetAvatarAsync(0, avatarSize);
        public async Awaitable<Texture2D> GetAvatarAsync(int retryCount, SteamAvatarSize avatarSize = SteamAvatarSize.Large)
        {
            if (_cachedAvatar != null) return _cachedAvatar;
            
            var handle = avatarSize switch
            {
                SteamAvatarSize.Small => SteamFriends.GetSmallFriendAvatar(Id),
                SteamAvatarSize.Medium => SteamFriends.GetMediumFriendAvatar(Id),
                SteamAvatarSize.Large => SteamFriends.GetLargeFriendAvatar(Id),
                _ => SteamFriends.GetLargeFriendAvatar(Id)
            };
            
            switch (handle)
            {
                case -1:
                    if (retryCount >= 5)
                    {
                        Debug.LogError("Failed to retrieve steam avatar after 5 attempts");
                        return null;
                    }
                    await Awaitable.NextFrameAsync();
                    return await GetAvatarAsync(++retryCount, avatarSize);
                case 0:
                    Debug.LogWarning("User has no steam avatar");
                    return null;
            }

            if (!SteamUtils.GetImageSize(handle, out var width, out var height))
            {
                Debug.LogError("Failed to retrieve avatar image size");
                return null;
            }
            
            var imageBytes = new byte[width * height * 4];
            if (!SteamUtils.GetImageRGBA(handle, imageBytes, (int)(width * height * 4)))
            {
                Debug.LogError("Failed to retrieve avatar image data");
                return null;
            }
            
            var tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(imageBytes);
            
            //Flipping the texture
            var pixels = tex.GetPixels();
            var flippedPixels = new Color[pixels.Length];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    flippedPixels[x + y * width] = pixels[x + (height - 1 - y) * width];
                }
            }
            tex.SetPixels(flippedPixels);
            tex.Apply();
            _cachedAvatar = tex;
            return tex;
        }

        public override string ToString() => $"{Name} ({Steam64Id})";
    }

    public enum SteamAvatarSize
    {
        Small,
        Medium,
        Large
    }
}
