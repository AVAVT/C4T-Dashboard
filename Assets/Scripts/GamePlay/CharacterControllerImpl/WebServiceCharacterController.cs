using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class WebServiceCharacterController : ICharacterController
{
  private static string TIMEOUT_MESSAGE = "request timeout";
  private Character character;
  public Character Character { get => character; set => character = value; }

  private bool isTimedOut = false;
  public bool IsTimedOut => isTimedOut;

  private bool isCrashed = false;
  public bool IsCrashed => isCrashed;

  public async Task DoStart(GameState gameState, GameConfig gameRule)
  {
    WWWForm form = new WWWForm();
    form.AddField("gameState", JsonConvert.SerializeObject(gameState));
    form.AddField("gameRule", JsonConvert.SerializeObject(gameRule));
    form.AddField("team", (int)character.team);
    form.AddField("role", (int)character.role);

    UnityWebRequest www = UnityWebRequest.Post("http://localhost:8686/start", form);
    www.timeout = 1;
    await www.SendWebRequest();

    // TODO timeout
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
    form.AddField("gameState", JsonConvert.SerializeObject(gameState));
    form.AddField("gameRule", JsonConvert.SerializeObject(gameRule));
    form.AddField("team", character.team.ToString());
    form.AddField("role", character.role.ToString());

    UnityWebRequest www = UnityWebRequest.Post("http://localhost:8686/turn", form);
    www.timeout = 1;
    await www.SendWebRequest();

    // TODO timeout
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
