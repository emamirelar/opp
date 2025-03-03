export enum InteractionType {
  Email = 'Email',
  Chat = 'Chat',
  Phone = 'Phone',
  VideoMeeting = 'VideoMeeting',
  InPersonMeeting = 'InPersonMeeting'
}

export const INTERACTION_TYPE_TRANSLATION_KEYS: Record<InteractionType, string> = {
  [InteractionType.Email]: 'label.interaction.types.email',
  [InteractionType.Chat]: 'label.interaction.types.chat',
  [InteractionType.Phone]: 'label.interaction.types.phone',
  [InteractionType.VideoMeeting]: 'label.interaction.types.video_meeting',
  [InteractionType.InPersonMeeting]: 'label.interaction.types.in_person_meeting'
};