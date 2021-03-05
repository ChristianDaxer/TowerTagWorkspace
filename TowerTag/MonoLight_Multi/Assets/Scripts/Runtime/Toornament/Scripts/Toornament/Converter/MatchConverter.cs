using System.Linq;
using Toornament.DataTransferObject;
using Toornament.Store.Model;

namespace Toornament.Converter {
    public static class MatchConverter {
        public static Match Convert(MatchDTO dto) {
            return new Match {
                id = dto.id,
                type = dto.type,
                discipline = dto.discipline,
                status = dto.status,
                tournament_id = dto.tournament_id,
                number = dto.number,
                stage_number = dto.stage_number,
                group_number = dto.group_number,
                round_number = dto.round_number,
                date = dto.date,
                timezone = dto.timezone,
                match_format = dto.match_format,
                note = dto.note,
                opponents = dto.opponents?.Select(Convert).ToArray(),
                streams = dto.streams?.Select(Convert).ToArray(),
                vods = dto.vods?.Select(Convert).ToArray(),
                private_note = dto.private_note
            };
        }

        public static MatchResult Convert(MatchResultDTO dto) {
            if (dto == null) return null;
            return new MatchResult {
                status = dto.status,
                opponents = dto.opponents?.Select(Convert).ToArray()
            };
        }

        public static MatchResultDTO Convert(MatchResult matchResult) {
            if (matchResult == null) return null;
            return new MatchResultDTO {
                status = matchResult.status,
                opponents = matchResult.opponents?.Select(Convert).ToArray()
            };
        }

        private static Opponent Convert(OpponentDTO dto) {
            if (dto == null) return null;
            return new Opponent {
                forfeit = dto.forfeit,
                Number = dto.number,
                Participant = ParticipantConverter.Convert(dto.participant),
                result = dto.result,
                score = dto.score
            };
        }

        private static OpponentDTO Convert(Opponent opponent) {
            if (opponent == null) return null;
            return new OpponentDTO {
                forfeit = opponent.forfeit,
                number = opponent.Number,
                participant = ParticipantConverter.Convert(opponent.Participant),
                result = opponent.result,
                score = opponent.score
            };
        }

        private static Match.Stream Convert(MatchDTO.Stream dto) {
            if (dto == null) return null;
            return new Match.Stream {
                id = dto.id,
                language = dto.language,
                name = dto.name,
                url = dto.url
            };
        }

        private static Match.VOD Convert(MatchDTO.VOD dto) {
            if (dto == null) return null;
            return new Match.VOD {
                category = dto.category,
                language = dto.language,
                name = dto.name,
                url = dto.url
            };
        }

        public static PatchMatchDTO ConvertToPatchMatchDTO(Match match) {
            if (match == null) return null;
            return new PatchMatchDTO {
                date = match.date,
                match_format = match.match_format,
                note = match.note,
                private_note = match.private_note,
                streams = match.streams?.Select(stream => stream.id).ToArray(),
                timezone = match.timezone,
                vods = match.vods?.Select(ConvertToPatchMatchVODDTO).ToArray()
            };
        }

        private static PatchMatchDTO.VOD ConvertToPatchMatchVODDTO(Match.VOD vod) {
            if (vod == null) return null;
            return new PatchMatchDTO.VOD {
                category = vod.category,
                language = vod.language,
                name = vod.name,
                url = vod.url
            };
        }
    }
}