const express = require('express');
const bodyParser = require("body-parser");

const app = express();

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json({ extended: false }));

app.post('/start', (req, res) => {
  console.log(`DoStart for team ${req.body.team} - role ${req.body.role}`);
  res.send();
});

app.post('/turn', (req, res) => {
  console.log(`DoTurn for team ${req.body.team} - role ${req.body.role}`);
  res.send('DOWN');
});

app.post('/timeout', (req, res) => {
  console.log(`Timeout for team ${req.body.team} - role ${req.body.role}`);
});

app.post('/crash', (req, res) => {
  console.log(`Crash for team ${req.body.team} - role ${req.body.role}`);
  res.sendStatus(500);
});

const port = process.env.port || 8686;

app.listen(port, err => {
  if (err) console.log(err);
  console.log("AI Server started at port " + port);
});