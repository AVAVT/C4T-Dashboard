﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class WebServiceDecisionMaker : ICharacterDescisionMaker
{
  private static string TIMEOUT_MESSAGE = "request timeout";
  private Character character;
  public Character Character { get => character; set => character = value; }

  private bool isTimedOut = false;
  public bool IsTimedOut => isTimedOut;

  private bool isCrashed = false;
  public bool IsCrashed => isCrashed;

  private string host = "";
  private string startPath = "";
  private string turnPath = "";

  public WebServiceDecisionMaker(string host, string startPath = "/start", string turnPath = "/turn")
  {
    this.host = host;
    this.startPath = startPath;
    this.turnPath = turnPath;
  }

  public async Task DoStart(GameState gameState, GameConfig gameRule)
  {
    WWWForm form = new WWWForm();
    form.AddField("data", JsonConvert.SerializeObject(new WebServiceTurnData()
    {
      gameRule = gameRule,
      gameState = gameState,
      team = (int)character.team,
      role = (int)character.role
    }));

    UnityWebRequest www = UnityWebRequest.Post($"{host}{startPath}", form);
    www.timeout = 1;
    await www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError)
    {
      if (www.error.ToLower() == TIMEOUT_MESSAGE) isTimedOut = true;
      else isCrashed = true;

      Debug.Log(www.error);
    }
  }

  public async Task<string> DoTurn(GameState gameState, GameConfig gameRule)
  {
    if (isCrashed || isTimedOut) return Directions.STAY;

    WWWForm form = new WWWForm();
    form.AddField("data", JsonConvert.SerializeObject(new WebServiceTurnData()
    {
      gameRule = gameRule,
      gameState = gameState,
      team = (int)character.team,
      role = (int)character.role
    }));

    UnityWebRequest www = UnityWebRequest.Post($"{host}{turnPath}", form);
    www.timeout = 1;
    await www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError)
    {
      if (www.error.ToLower() == TIMEOUT_MESSAGE) isTimedOut = true;
      else isCrashed = true;
      return Directions.STAY;
    }
    else
    {
      return www.downloadHandler.text;
    }
  }
}