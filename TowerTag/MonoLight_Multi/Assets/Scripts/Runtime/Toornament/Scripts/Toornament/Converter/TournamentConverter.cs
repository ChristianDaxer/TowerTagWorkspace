using System.Linq;
using Toornament.DataTransferObject;
using Toornament.Store.Model;

namespace Toornament.Converter {
    public static class TournamentConverter {
        public static Tournament Convert(TournamentDTO dto) {
            return new Tournament {
                id = dto.id,
                discipline = dto.discipline,
                name = dto.name,
                full_name = dto.full_name,
                status = dto.status,
                date_start = dto.date_start,
                date_end = dto.date_end,
                timezone = dto.timezone,
                online = dto.online,
                _public = dto._public,
                archived = dto.archived,
                location = dto.location,
                country = dto.country,
                size = dto.size,
                participant_type = dto.participant_type,
                match_type = dto.match_type,
                organization = dto.organization,
                website = dto.website,
                description = dto.description,
                rules = dto.rules,
                prize = dto.prize,
                streams = dto.streams?.Select(Convert).ToArray(),
                platforms = dto.platforms,
                logo = Convert(dto.logo),
                check_in = dto.check_in,
                participant_nationality = dto.participant_nationality,
                match_format = dto.match_format
            };
        }

        private static Tournament.Logo Convert(TournamentDTO.Logo logo) {
            if (logo == null) return null;
            return new Tournament.Logo {
                logo_large = logo.logo_large,
                logo_medium = logo.logo_medium,
                logo_small = logo.logo_small,
                original = logo.original
            };
        }

        private static Tournament.TournamentStream Convert(TournamentDTO.TournamentStream stream) {
            if (stream == null) return null;
            return new Tournament.TournamentStream {
                id = stream.id,
                language = stream.language,
                name = stream.name,
                url = stream.url
            };
        }

        public static CreateTournamentDTO ConvertToCreationDTO(Tournament tournament) {
            if (tournament == null) return null;
            return new CreateTournamentDTO {
                discipline = tournament.discipline,
                name = tournament.name,
                size = tournament.size,
                participant_type = tournament.participant_type,
                full_name = tournament.full_name,
                organization = tournament.organization,
                website = tournament.website,
                date_start = tournament.date_start,
                date_end = tournament.date_end,
                timezone = tournament.timezone,
                online = tournament.online,
                _public = tournament._public,
                location = tournament.location,
                country = tournament.country,
                description = tournament.description,
                rules = tournament.rules,
                prize = tournament.prize,
                check_in = tournament.check_in,
                participant_nationality = tournament.participant_nationality,
                match_format = tournament.match_format,
                platforms = tournament.platforms
            };
        }
    }
}