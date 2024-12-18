import { Terrain } from '~/models/strategus/terrain'

export const TerrainColorByType: Record<Terrain, string> = {
  [Terrain.Forest]: '#047857',
  [Terrain.Mountain]: '#d1d5db ',
  [Terrain.River]: '#0284c7',
}
