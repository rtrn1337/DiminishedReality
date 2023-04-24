using UnityEngine;
using System.Collections;

namespace GRT
{
    public interface IGrtDataHandler
    {
        void HandleGrtData(ref RequestHelper.InPaintData data);
    }
}
