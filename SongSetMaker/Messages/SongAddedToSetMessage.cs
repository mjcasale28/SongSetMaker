using CommunityToolkit.Mvvm.Messaging.Messages;
using SongSetMaker.Models;

namespace SongSetMaker.Messages
{
    public class SongAddedToSetMessage : ValueChangedMessage<Song>
    {
        public SongAddedToSetMessage(Song song) : base(song) { }
    }
}
