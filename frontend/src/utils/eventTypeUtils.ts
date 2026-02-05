import type { EventType } from '../types/types'

export interface EventTypeInfo {
  displayName: string
  emoji: string
}

/**
 * Maps event type to display name and emoji
 */
export function getEventTypeInfo(eventType: EventType): EventTypeInfo {
  const mapping: Record<EventType, EventTypeInfo> = {
    Wedding: { displayName: 'Wesele', emoji: '💍' },
    Birthday: { displayName: 'Urodziny', emoji: '🎂' },
    Baptism: { displayName: 'Chrzest', emoji: '👶' },
    Communion: { displayName: 'Komunia', emoji: '⛪' },
    Corporate: { displayName: 'Wydarzenie firmowe', emoji: '🏢' },
    Conference: { displayName: 'Konferencja', emoji: '🎤' },
    Other: { displayName: 'Wydarzenie', emoji: '🎉' }
  }

  return mapping[eventType] || mapping.Other
}
