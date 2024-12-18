import qs from 'qs'

import type { ActivityLog, ActivityLogType } from '~/models/activity-logs'
import type { UserPublic } from '~/models/user'

import { get } from '~/services/crpg-client'
import { getUsersByIds } from '~/services/users-service'

export interface ActivityLogsPayload {
  to: Date
  from: Date
  userId: number[]
  type?: ActivityLogType[]
}

export const getActivityLogs = async (payload: ActivityLogsPayload) =>
  get<ActivityLog[]>(
    `/activity-logs?${qs.stringify(payload, {
      arrayFormat: 'brackets',
      skipNulls: true,
      strictNullHandling: true,
    })}`,
  )

const extractUsersFromLogs = (logs: ActivityLog[]) =>
  logs.reduce((out, l) => {
    if ('targetUserId' in l.metadata) {
      out.push(Number(l.metadata.targetUserId))
    }

    if ('actorUserId' in l.metadata) {
      out.push(Number(l.metadata.actorUserId))
    }

    return [...new Set(out)]
  }, [] as number[])

export const getActivityLogsWithUsers = async (payload: ActivityLogsPayload) => {
  const logs = await getActivityLogs(payload)

  const users = (
    await getUsersByIds([...new Set([...payload.userId, ...extractUsersFromLogs(logs)])])
  ).reduce(
    (out, user) => {
      out[user.id] = user
      return out
    },
    {} as Record<number, UserPublic>,
  )

  return {
    logs,
    users,
  }
}
