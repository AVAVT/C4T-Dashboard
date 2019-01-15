import random


class GardenBotBase():
    def __init__(self, tile_types, directions, valid_moves, teams, roles):
        self.tile_types = tile_types
        self.directions = directions
        self.valid_moves = valid_moves
        self.teams = teams
        self.roles = roles


class GardenBot(GardenBotBase):
    def get_name(self):
        return "Random Bot"

    def do_start(self, game_rule, game_state):

        return

    def do_turn(self, game_rule, game_state):
        return random.choice(self.valid_moves)
