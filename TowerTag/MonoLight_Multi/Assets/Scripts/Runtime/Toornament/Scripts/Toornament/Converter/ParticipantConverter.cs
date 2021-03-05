using System.Linq;
using Toornament.DataTransferObject;
using Toornament.Store.Model;

namespace Toornament.Converter {
    public static class ParticipantConverter {
        public static Participant Convert(ParticipantDTO dto) {
            if (dto == null) return null;
            return new Participant {
                check_in = dto.check_in,
                country = dto.country,
                custom_fields = dto.custom_fields?.Select(Convert).ToArray(),
                custom_fields_private = dto.custom_fields_private?.Select(Convert).ToArray(),
                email = dto.email,
                id = dto.id,
                lineup = dto.lineup?.Select(Convert).ToArray(),
                logo = Convert(dto.logo),
                name = dto.name
            };
        }

        public static ParticipantDTO Convert(Participant participant) {
            if (participant == null) return null;
            return new ParticipantDTO {
                check_in = participant.check_in,
                country = participant.country,
                custom_fields = participant.custom_fields?.Select(Convert).ToArray(),
                custom_fields_private = participant.custom_fields_private?.Select(Convert).ToArray(),
                email = participant.email,
                id = participant.id,
                lineup = participant.lineup?.Select(Convert).ToArray(),
                logo = Convert(participant.logo),
                name = participant.name
            };
        }

        private static Participant.CustomField Convert(ParticipantDTO.CustomField dto) {
            if (dto == null) return null;
            return new Participant.CustomField {
                label = dto.label,
                type = dto.type,
                value = dto.value
            };
        }

        private static ParticipantDTO.CustomField Convert(Participant.CustomField dto) {
            if (dto == null) return null;
            return new ParticipantDTO.CustomField {
                label = dto.label,
                type = dto.type,
                value = dto.value
            };
        }

        private static Participant.Lineup Convert(ParticipantDTO.Lineup dto) {
            if (dto == null) return null;
            return new Participant.Lineup {
                name = dto.name,
                country = dto.country,
                custom_fields = dto.custom_fields?.Select(Convert).ToArray(),
                email = dto.email,
                custom_fields_private = dto.custom_fields_private?.Select(Convert).ToArray()
            };
        }

        private static ParticipantDTO.Lineup Convert(Participant.Lineup dto) {
            if (dto == null) return null;
            return new ParticipantDTO.Lineup {
                name = dto.name,
                country = dto.country,
                custom_fields = dto.custom_fields?.Select(Convert).ToArray(),
                email = dto.email,
                custom_fields_private = dto.custom_fields_private?.Select(Convert).ToArray()
            };
        }

        private static Participant.Logo Convert(ParticipantDTO.Logo dto) {
            if (dto == null) return null;
            return new Participant.Logo {
                icon_large_square = dto.icon_large_square,
                extra_small_square = dto.extra_small_square,
                medium_small_square = dto.medium_small_square,
                medium_large_square = dto.medium_large_square
            };
        }

        private static ParticipantDTO.Logo Convert(Participant.Logo dto) {
            if (dto == null) return null;
            return new ParticipantDTO.Logo {
                icon_large_square = dto.icon_large_square,
                extra_small_square = dto.extra_small_square,
                medium_small_square = dto.medium_small_square,
                medium_large_square = dto.medium_large_square
            };
        }
    }
}