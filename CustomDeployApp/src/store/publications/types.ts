// Tipos para o gerenciamento de publicações

export interface Publication {
  name: string;
  siteName?: string;
  subApplication?: string;
  repository?: string;
  branch?: string;
  buildCommand?: string;
  buildOutput?: string;
  targetPath: string;
  sizeMB: number;
  deployedAt?: string;
  hasMetadata: boolean;
  exists: boolean;
  isSubProject: boolean;
  projectRoot: string;
  repoUrl?: string;
}

export interface PublicationStats {
  totalPublications: number;
  sites: number;
  applications: number;
  withMetadata: number;
  withoutMetadata: number;
  totalSizeMB: number;
  averageSizeMB: number;
  latestDeployment?: Publication;
  oldestDeployment?: Publication;
  largestPublication?: Publication;
  smallestPublication?: Publication;
  sitesWithoutMetadata: Publication[];
}

export interface CreateMetadataRequest {
  iisSiteName: string;
  subPath?: string;
  repoUrl: string;
  branch: string;
  buildCommand: string;
  buildOutput: string;
}

export interface UpdateMetadataRequest {
  repository?: string;
  branch?: string;
  buildCommand?: string;
  buildOutput?: string;
}

export interface PublicationsState {
  publications: Publication[];
  stats: PublicationStats | null;
  loading: {
    publications: boolean;
    stats: boolean;
    create: boolean;
    update: boolean;
    delete: boolean;
  };
  error: {
    publications: string | null;
    stats: string | null;
    create: string | null;
    update: string | null;
    delete: string | null;
  };
}

// Action types
export const PUBLICATIONS_ACTION_TYPES = {
  // Fetch publications
  FETCH_PUBLICATIONS_START: 'publications/fetchPublicationsStart',
  FETCH_PUBLICATIONS_SUCCESS: 'publications/fetchPublicationsSuccess',
  FETCH_PUBLICATIONS_FAILURE: 'publications/fetchPublicationsFailure',
  
  // Fetch stats
  FETCH_STATS_START: 'publications/fetchStatsStart',
  FETCH_STATS_SUCCESS: 'publications/fetchStatsSuccess',
  FETCH_STATS_FAILURE: 'publications/fetchStatsFailure',
  
  // Create metadata
  CREATE_METADATA_START: 'publications/createMetadataStart',
  CREATE_METADATA_SUCCESS: 'publications/createMetadataSuccess',
  CREATE_METADATA_FAILURE: 'publications/createMetadataFailure',
  
  // Update metadata
  UPDATE_METADATA_START: 'publications/updateMetadataStart',
  UPDATE_METADATA_SUCCESS: 'publications/updateMetadataSuccess',
  UPDATE_METADATA_FAILURE: 'publications/updateMetadataFailure',
  
  // Delete publication
  DELETE_PUBLICATION_START: 'publications/deletePublicationStart',
  DELETE_PUBLICATION_SUCCESS: 'publications/deletePublicationSuccess',
  DELETE_PUBLICATION_FAILURE: 'publications/deletePublicationFailure',
  
  // Clear errors
  CLEAR_ERRORS: 'publications/clearErrors',
} as const;
