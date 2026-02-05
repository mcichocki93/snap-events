import { ref, readonly } from 'vue'

export interface Notification {
  id: number
  type: 'positive' | 'negative' | 'warning' | 'info'
  message: string
  timeout?: number
}

const notifications = ref<Notification[]>([])
let notificationId = 0

export function useNotification() {
  const notify = (options: {
    type?: 'positive' | 'negative' | 'warning' | 'info'
    message: string
    timeout?: number
  }) => {
    const id = ++notificationId
    const notification: Notification = {
      id,
      type: options.type || 'info',
      message: options.message,
      timeout: options.timeout ?? 3000
    }

    notifications.value.push(notification)

    // Auto remove after timeout
    if (notification.timeout > 0) {
      setTimeout(() => {
        removeNotification(id)
      }, notification.timeout)
    }

    return id
  }

  const removeNotification = (id: number) => {
    const index = notifications.value.findIndex(n => n.id === id)
    if (index !== -1) {
      notifications.value.splice(index, 1)
    }
  }

  return {
    notifications: readonly(notifications),
    notify,
    removeNotification
  }
}
