export enum InteractionType {
  Email = 'Email',
  Chat = 'Chat',
  Call = 'Call',
  VirtualMeeting = 'VirtualMeeting',
  InPersonMeeting = 'InPersonMeeting'
}

export const INTERACTION_TYPE_TRANSLATION_KEYS: Record<InteractionType, string> = {
  [InteractionType.Email]: 'label.interaction.types.email',
  [InteractionType.Chat]: 'label.interaction.types.chat',
  [InteractionType.Call]: 'label.interaction.types.call',
  [InteractionType.VirtualMeeting]: 'label.interaction.types.virtual_meeting',
  [InteractionType.InPersonMeeting]: 'label.interaction.types.in_person_meeting'
};
