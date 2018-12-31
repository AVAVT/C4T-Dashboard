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

  public string PlayerName { get; private set; }
  public bool IsReady { get; private set; }

  private bool isTimedOut = false;
  public bool IsTimedOut => isTimedOut;

  private bool isCrashed = false;
  public bool IsCrashed => isCrashed;

  private string host = "";
  private string startPath = "";
  private string turnPath = "";
  private string namePath = "";

  public WebServiceDecisionMaker(string host, string startPath = "/start", string turnPath = "/turn", string namePath = "/name")
  {
    this.host = host;
    this.startPath = startPath;
    this.turnPath = turnPath;
    this.PlayerName = "UNKNOWN";
    this.namePath = namePath;
    IsReady = false;
  }

  public async Task DoStart(GameState gameState, GameConfig gameRule)
  {
    try
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
      var asyncOp = www.SendWebRequest();
      while (!asyncOp.isDone) await Task.Delay(1);

      if (www.isNetworkError || www.isHttpError)
      {
        isCrashed = true;
        Debug.Log(www.error);
      }
      else
      {
        var result = www.downloadHandler.text;
        if (result.ToLower() == TIMEOUT_MESSAGE) isTimedOut = true;
      }
    }
    catch (System.Exception e) { throw e; }
  }

  public async Task<string> DoTurn(GameState gameState, GameConfig gameRule)
  {
    if (isCrashed || isTimedOut) return Directions.STAY;

    try
    {
      WWWForm form = new WWWForm();
      form.AddField("data", JsonConvert.SerializeObject(new WebServiceTurnData()
      {
        gameRule = gameRule,
        gameState = gameState,
        team = (int)character.team,
        role = (int)character.role
      }));

      UnityWebRequest www = UnityWebRequest.Post($"{host}{turnPath}", form);
      var asyncOp = www.SendWebRequest();
      while (!asyncOp.isDone) await Task.Delay(1);

      if (www.isNetworkError || www.isHttpError)
      {
        isCrashed = true;
        return Directions.STAY;
      }
      else
      {
        var result = www.downloadHandler.text;
        if (result.ToLower() == TIMEOUT_MESSAGE)
        {
          isTimedOut = true;
          return Directions.STAY;
        }
        else return result;
      }
    }
    catch (System.Exception e) { throw e; }
  }

  public async Task Handshake(int team, int role)
  {
    try
    {
      WWWForm form = new WWWForm();
      form.AddField("team", team);
      form.AddField("role", role);

      UnityWebRequest www = UnityWebRequest.Post($"{host}{namePath}", form);
      var asyncOp = www.SendWebRequest();
      while (!asyncOp.isDone) await Task.Delay(1);

      if (www.isNetworkError || www.isHttpError)
      {
        IsReady = false;
      }
      else
      {
        PlayerName = www.downloadHandler.text;
        IsReady = true;
      }
    }
    catch (System.Exception e) { throw e; }
  }
}