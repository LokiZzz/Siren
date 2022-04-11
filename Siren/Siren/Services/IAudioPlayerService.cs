using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public interface IAudioPlayerService
    {
        Task Load(string filePath);

        void PlayPause();
    }
}
