const express = require('express');
const bodyParser = require("body-parser");

const app = express();

const validActions = [
  "UP",
  "DOWN",
  "LEFT",
  "RIGHT",
  "STAY"
];

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json({ extended: false }));

app.post('/start', (req, res) => {
  const data = JSON.parse(req.body.data);
  console.log(`DoStart test for team ${data.team} - role ${data.role}`);
  res.send();
});

app.post('/turn', (req, res) => {
  const data = JSON.parse(req.body.data);
  console.log(`DoTurn ${data.gameState.turn} test for team ${data.team} - role ${data.role}`);
  res.send('DOWN');
});

app.post('/name', (req, res) => {
  console.log(`Name test`);
  res.send("Test Player Name");
});

const port = process.env.port || 8484;

app.listen(port, err => {
  if (err) console.log(err);
  console.log("AI Server started at port " + port);
  console.log("Exceptions & errors will be printed in this window.");
});