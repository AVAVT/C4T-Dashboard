const express = require('express');
const bodyParser = require("body-parser");

const app = express();

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json({ extended: false }));

app.post('/start', (req, res) => {
  console.log(JSON.parse(req.body.gameState).turn);
  res.send();
});

app.post('/turn', (req, res) => {
  console.log(JSON.parse(req.body.gameState).turn);
  res.send('DOWN');
});

const port = process.env.port || 8686;

app.listen(port, err => {
  if (err) console.log(err);
  console.log("AI Server started at port " + port);
});