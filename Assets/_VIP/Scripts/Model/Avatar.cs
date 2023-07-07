﻿using KBEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    GAME_PREPARE,
    GAME_START,
    GAME_OVER
}
public class Avatar : AvatarBase
{
    public GameState gameState;
    public int frameId;
    public List<FRAME_SYNC> frames = new List<FRAME_SYNC>();

    public static Avatar Player
    {
        get
        {
            return (KBEngine.KBEngineApp.app.player() as Avatar);
        }
    }
    public override void __init__()
    {
        base.__init__();
        gameState = GameState.GAME_PREPARE;
        frameId = 0;
        if (isPlayer())
        {
            KBEngine.Event.registerIn("EnterRoom", this, nameof(EnterRoom));
            KBEngine.Event.registerIn("PlaceCard", this, nameof(PlaceCard));
        }
    }

    public void PlaceCard(CMD cmd)
    {
        cellEntityCall.PlaceCard(cmd);
    }

    public void EnterRoom()
    {
        baseEntityCall.EnterRoom();
    }

    #region CallBack
    public override void OnFrameSync(int frameId, FRAME_SYNC fs)
    {
        Dbg.INFO_MSG($"OnFrameSync{frameId},{fs}");
        //生产帧
        this.frameId = frameId;
        this.frames.Add(fs);
    }

    public override void OnGameOver()
    {
        Dbg.INFO_MSG("房间帧已跑完");
    }

    public override void OnGameReady()
    {
        Dbg.INFO_MSG("房间帧已匹配完毕");
        gameState = GameState.GAME_START;
        frameId = 0;

        KBEngine.Event.fireOut("OnGameReady");
    }
    #endregion

}
