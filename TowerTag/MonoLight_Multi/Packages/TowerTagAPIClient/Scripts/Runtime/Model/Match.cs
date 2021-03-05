using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Random = UnityEngine.Random;

namespace TowerTagAPIClient.Model {
    public class Match {
        // meta data (filled out by server)
        public string id = "";
        public string apiVersion;
        public DateTime date = DateTime.Now;
        public string location;
        public string appVersion;
        public bool basicMode;
        public string roomName;

        // match setup
        public Player[] players;
        public Team[] teams;
        public int matchTime;
        public string gameMode;
        public string map;
        public DateTime startTime;

        // results
        public int winningTeam;
        public int rounds;
        public Dictionary<int, int> teamScores;
        public Dictionary<string, PlayerPerformance> playerPerformances;
        public List<Round> roundDetails;

        public class Player {
            public string id;
            public int teamId;
            public bool isBot;
            public bool isMember;
        }

        public class Team {
            [DefaultValue(-1)]
            public int id;
            public string name;
            public string[] players;
        }

        public class PlayerPerformance {
            public int score;
            public int outs;
            public int assists;
            public int shotsFired;
            public int hitsDealt;
            public int hitsTaken;
            public int damageDealt;
            public int damageTaken;
            public int healthHealed;
            public int healingReceived;
            public int goalPillarsClaimed;
            public int pillarsClaimed;
            public int teleports;
            public float playTime;

            // reward typos
            public int headshots;
            public int snipershots;
            public int doubles;

            public string commendation;
        }

        public class Round {
            public int winningTeam;
            public int playTimeInSeconds;
        }

        public static Match DummyMatch(Player[] players) {
            Dictionary<string, PlayerPerformance> playerPerformances = players.ToDictionary(
                player => player.id, DummyPerformance);
            return new Match {
                location = "TestLocation",
                date = DateTime.Now,
                roomName = "OJTest",
                basicMode = true,
                appVersion = "2019.2_test",
                players = players,
                teams = new[] {
                    new Team {id = 0, name = "Fire", players = players
                        .Where(p => p.teamId == 0)
                        .Select(p => p.id).ToArray()},
                    new Team {id = 1, name = "Ice", players = players
                        .Where(p => p.teamId == 1)
                        .Select(p => p.id).ToArray()}
                },
                matchTime = 480,
                startTime = DateTime.Now.Subtract(TimeSpan.FromSeconds(480)),
                gameMode = "DeathMatch",
                map = "Maze",
                winningTeam = 1,
                rounds = 2,
                roundDetails = new List<Round> {
                    new Round {
                        playTimeInSeconds = 240,
                        winningTeam = 1
                    },
                    new Round {
                        playTimeInSeconds = 240,
                        winningTeam = -1
                    }
                },
                teamScores = new Dictionary<int, int> {{0, 5}, {1, 7}},
                playerPerformances = playerPerformances
            };
        }

        public static PlayerPerformance DummyPerformance(Player player) {
            return new PlayerPerformance {
                score = Random.Range(0, 20),
                outs = Random.Range(0, 20),
                assists = Random.Range(0, 20),
                shotsFired = Random.Range(100, 1000),
                hitsDealt = Random.Range(10, 100),
                hitsTaken = Random.Range(10, 100),
                damageDealt = Random.Range(400, 800),
                damageTaken = Random.Range(400, 800),
                healthHealed = Random.Range(0, 100),
                healingReceived = Random.Range(0, 100),
                goalPillarsClaimed = 0,
                pillarsClaimed = Random.Range(0, 20),
                teleports = Random.Range(50, 200),
                playTime = Random.Range(240, 480),
                headshots = Random.Range(0, 10),
                snipershots = Random.Range(0, 10),
                doubles = Random.Range(0, 10),
                commendation = ""
            };
        }
    }
}