using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageCollisionVersion
{
    Invalid = 0,
    HKX = 1, //Kenzan havok proprietary collision
    SCTD = 2, //SCTD, OOE games
    GCTD = 3, //GCTD, introduced on OE as .sct, Named .gct starting from DE
}
