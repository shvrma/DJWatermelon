using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal record LavalinkOptions(
    string WebSocketUri, 
    string Authorization, 
    string UserId);
