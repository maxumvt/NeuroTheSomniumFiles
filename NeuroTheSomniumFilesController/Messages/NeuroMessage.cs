using System.Collections.Generic;
using System.Linq;

namespace NeuroTheSomniumFiles;

public class NeuroMessage
{
    public Dictionary<string, object> message = new Dictionary<string, object>()
    {
        {"command", "startup"},
        {"game","AI The Somnium Files"},
        {"data", null},

    };
}