export enum InteractionType {
  Email = 'Email',
  Chat = 'Chat',
  Phone = 'Phone',
  VirtualMeeting = 'VirtualMeeting',
  InPersonMeeting = 'InPersonMeeting'
}

export const INTERACTION_TYPE_TRANSLATION_KEYS: Record<InteractionType, string> = {
  [InteractionType.Email]: 'label.interaction.types.email',
  [InteractionType.Chat]: 'label.interaction.types.chat',
  [InteractionType.Phone]: 'label.interaction.types.phone',
  [InteractionType.VirtualMeeting]: 'label.interaction.types.virtual_meeting',
  [InteractionType.InPersonMeeting]: 'label.interaction.types.in_person_meeting'
};
