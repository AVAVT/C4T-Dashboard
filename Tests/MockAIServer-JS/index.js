const express = require('express');
const bodyParser = require("body-parser");

const app = express();

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json({ extended: false }));

app.post('/start', (req, res) => {
  const data = JSON.parse(req.body.data);
  console.log(`DoStart test for team ${data.team} - role ${data.role}`);
  res.send();
});

app.post('/turn', (req, res) => {
  const data = JSON.parse(req.body.data);
  console.log(`DoTurn test for turn ${data.gameState.turn} team ${data.team} - role ${data.role}`);
  res.send('DOWN');
});

app.post('/timeout', (req, res) => {
  const data = JSON.parse(req.body.data);
  res.send("request timeout");
  console.log(`Timeout test for team ${data.team} - role ${data.role}`);
});

app.post('/crash', (req, res) => {
  const data = JSON.parse(req.body.data);
  console.log(`Crash test for team ${data.team} - role ${data.role}`);
  res.sendStatus(500);
});

app.post('/name', (req, res) => {
  console.log(`Name test`);
  res.send("Test Player Name");
});

app.post('/namecrash', (req, res) => {
  console.log(`Name crash test`);
  res.sendStatus(500);
});

const port = process.env.port || 8686;

app.listen(port, err => {
  if (err) console.log(err);
  console.log("AI Server started at port " + port);
});