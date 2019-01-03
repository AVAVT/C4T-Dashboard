import tornado.ioloop
import tornado.web
import random
import json
from constants import valid_moves, directions, teams_arr, roles_arr

from Team1.Harvester.main import GardenBot as harvester1
from Team1.Planter.main import GardenBot as planter1
from Team1.Worm.main import GardenBot as worm1
from Team2.Harvester.main import GardenBot as harvester2
from Team2.Planter.main import GardenBot as planter2
from Team2.Worm.main import GardenBot as worm2

coin_flip = random.randint(0, 1) == 1

if(coin_flip):
    bots = [
        [
            planter1(directions, valid_moves, teams_arr, roles_arr),
            harvester1(directions, valid_moves, teams_arr, roles_arr),
            worm1(directions, valid_moves, teams_arr, roles_arr)
        ], [
            planter2(directions, valid_moves, teams_arr, roles_arr),
            harvester2(directions, valid_moves, teams_arr, roles_arr),
            worm2(directions, valid_moves, teams_arr, roles_arr)
        ]
    ]

else:
    bots = [
        [
            planter2(directions, valid_moves, teams_arr, roles_arr),
            harvester2(directions, valid_moves, teams_arr, roles_arr),
            worm2(directions, valid_moves, teams_arr, roles_arr)
        ], [
            planter1(directions, valid_moves, teams_arr, roles_arr),
            harvester1(directions, valid_moves, teams_arr, roles_arr),
            worm1(directions, valid_moves, teams_arr, roles_arr)
        ]
    ]


class StartHandler(tornado.web.RequestHandler):
    def post(self):
        data = json.loads(self.get_argument('data'))
        team = data["team"]
        role = data["role"]
        try:
            bot = bots[team][role]
            bot.do_start(data["gameRule"], data["gameState"])
            self.write("OK")
        except Exception as e:
            print(
                f"Exception occured in bot: {teams_arr[team]} {roles_arr[role]}")
            print(e)
            self.set_status(500)


class TurnHandler(tornado.web.RequestHandler):
    def post(self):
        data = json.loads(self.get_argument('data'))
        team = data["team"]
        role = data["role"]
        try:
            bot = bots[team][role]
            result = bot.do_turn(data["gameRule"], data["gameState"])
            if(result in valid_moves):
                self.write(result)
            else:
                raise Exception(
                    f"Bot returned '{result}', which is is not a valid move")
        except Exception as e:
            print(
                f"Exception occured in bot: {teams_arr[team]} {roles_arr[role]}")
            print(e)
            self.set_status(500)


class NameHandler(tornado.web.RequestHandler):
    def post(self):
        data = json.loads(self.get_argument('data'))
        team = data["team"]
        role = data["role"]
        try:
            name = bots[team][role].get_name()
            self.write(name)
        except Exception as e:
            print(
                f"Exception occured in bot: {teams_arr[team]} {roles_arr[role]}")
            print(e)
            self.set_status(500)


def make_app():
    return tornado.web.Application([
        ("/start", StartHandler),
        ("/turn", TurnHandler),
        ("/name", NameHandler)
    ])


if __name__ == "__main__":
    app = make_app()
    app.listen(8484)
    print("AI Server started at port 8484")
    print("Exceptions & errors will be printed in this window.")
    tornado.ioloop.IOLoop.current().start()
