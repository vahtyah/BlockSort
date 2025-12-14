using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonLevelData
{
    public int levelNumber;
    public int row;
    public int column;
    public int totalMystery;
    public int totalTunnel;
    public int totalConnectedBox;
    public int totalTwin;
    public int totalChained;
    public int totalRope;
    public bool isHardLevel;
    public List<JsonPiece> pieces;
    public List<JsonBox> boxes;
    public List<object> tunnels;
    public object twinBoxes;
}

[Serializable]
public class JsonPiece
{
    public string color;
    public bool isMystery;
    public bool isBasePiece;
    public int dataIndex;
    public int stackSize;
    public JsonVector3 position;
    public JsonQuaternion rotation;
}

[Serializable]
public class JsonBox
{
    public string color;
    public bool isMystery;
    public bool isTunnelBox;
    public bool isConnected;
    public bool isBaseConnector;
    public bool isTwin;
    public bool isChained;
    public bool isRopeConnected;
    public bool isRopeConnector;
    public string connectDirection;
    public string ropeType;
    public int connectTo;
    public int boxType;
    public int dataIndex;
    public int tunnelIndex;
    public int chainStrength;
    public int ropeConnectTo;
    public JsonVector3 position;
    public JsonVector3 scale;
    public JsonQuaternion rotation;
}

[Serializable]
public class JsonVector3
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToUnityVector3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class JsonQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Quaternion ToUnityQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}

