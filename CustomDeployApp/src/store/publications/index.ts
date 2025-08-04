import { createSlice } from '@reduxjs/toolkit';
import type { PublicationsState } from './types';
import {
  fetchPublications,
  fetchPublicationByName,
  fetchPublicationsStats,
  fetchPublicationMetadata,
  createMetadata,
  updateMetadata,
  deletePublication,
  deletePublicationMetadataOnly,
  clearErrors
} from './actions';

const initialState: PublicationsState = {
  publications: [],
  stats: null,
  loading: {
    publications: false,
    stats: false,
    create: false,
    update: false,
    delete: false,
  },
  error: {
    publications: null,
    stats: null,
    create: null,
    update: null,
    delete: null,
  },
};

const publicationsSlice = createSlice({
  name: 'publications',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    // Fetch Publications
    builder
      .addCase(fetchPublications.pending, (state) => {
        state.loading.publications = true;
        state.error.publications = null;
      })
      .addCase(fetchPublications.fulfilled, (state, action) => {
        state.loading.publications = false;
        state.publications = action.payload.publications;
        state.error.publications = null;
      })
      .addCase(fetchPublications.rejected, (state, action) => {
        state.loading.publications = false;
        state.error.publications = action.payload as string;
      });

    // Fetch Publication by Name
    builder
      .addCase(fetchPublicationByName.pending, (state) => {
        state.loading.publications = true;
        state.error.publications = null;
      })
      .addCase(fetchPublicationByName.fulfilled, (state, action) => {
        state.loading.publications = false;
        // Atualizar ou adicionar publicação na lista
        const existingIndex = state.publications.findIndex(
          p => p.name === action.payload.publication.name
        );
        if (existingIndex >= 0) {
          state.publications[existingIndex] = action.payload.publication;
        } else {
          state.publications.push(action.payload.publication);
        }
        state.error.publications = null;
      })
      .addCase(fetchPublicationByName.rejected, (state, action) => {
        state.loading.publications = false;
        state.error.publications = action.payload as string;
      });

    // Fetch Publications Stats
    builder
      .addCase(fetchPublicationsStats.pending, (state) => {
        state.loading.stats = true;
        state.error.stats = null;
      })
      .addCase(fetchPublicationsStats.fulfilled, (state, action) => {
        state.loading.stats = false;
        state.stats = action.payload.stats;
        state.error.stats = null;
      })
      .addCase(fetchPublicationsStats.rejected, (state, action) => {
        state.loading.stats = false;
        state.error.stats = action.payload as string;
      });

    // Fetch Publication Metadata
    builder
      .addCase(fetchPublicationMetadata.pending, (state) => {
        state.loading.publications = true;
        state.error.publications = null;
      })
      .addCase(fetchPublicationMetadata.fulfilled, (state, action) => {
        state.loading.publications = false;
        // Atualizar publicação específica na lista
        const existingIndex = state.publications.findIndex(
          p => p.name === action.payload.metadata.name
        );
        if (existingIndex >= 0) {
          state.publications[existingIndex] = action.payload.metadata;
        }
        state.error.publications = null;
      })
      .addCase(fetchPublicationMetadata.rejected, (state, action) => {
        state.loading.publications = false;
        state.error.publications = action.payload as string;
      });

    // Create Metadata
    builder
      .addCase(createMetadata.pending, (state) => {
        state.loading.create = true;
        state.error.create = null;
      })
      .addCase(createMetadata.fulfilled, (state, action) => {
        state.loading.create = false;
        // Adicionar nova publicação à lista
        state.publications.push(action.payload);
        state.error.create = null;
      })
      .addCase(createMetadata.rejected, (state, action) => {
        state.loading.create = false;
        state.error.create = action.payload as string;
      });

    // Update Metadata
    builder
      .addCase(updateMetadata.pending, (state) => {
        state.loading.update = true;
        state.error.update = null;
      })
      .addCase(updateMetadata.fulfilled, (state, action) => {
        state.loading.update = false;
        // Atualizar publicação na lista
        const existingIndex = state.publications.findIndex(
          p => p.name === action.payload.updatedMetadata.name
        );
        if (existingIndex >= 0) {
          state.publications[existingIndex] = action.payload.updatedMetadata;
        }
        state.error.update = null;
      })
      .addCase(updateMetadata.rejected, (state, action) => {
        state.loading.update = false;
        state.error.update = action.payload as string;
      });

    // Delete Publication
    builder
      .addCase(deletePublication.pending, (state) => {
        state.loading.delete = true;
        state.error.delete = null;
      })
      .addCase(deletePublication.fulfilled, (state, action) => {
        state.loading.delete = false;
        // Remover publicação da lista
        state.publications = state.publications.filter(
          p => p.name !== action.payload.name
        );
        state.error.delete = null;
      })
      .addCase(deletePublication.rejected, (state, action) => {
        state.loading.delete = false;
        state.error.delete = action.payload as string;
      });

    // Delete Publication Metadata Only
    builder
      .addCase(deletePublicationMetadataOnly.pending, (state) => {
        state.loading.delete = true;
        state.error.delete = null;
      })
      .addCase(deletePublicationMetadataOnly.fulfilled, (state, action) => {
        state.loading.delete = false;
        // Atualizar publicação para indicar que não tem mais metadados
        const existingIndex = state.publications.findIndex(
          p => p.name === action.payload.name
        );
        if (existingIndex >= 0) {
          state.publications[existingIndex] = {
            ...state.publications[existingIndex],
            hasMetadata: false,
            repository: undefined,
            branch: undefined,
            buildCommand: undefined,
            deployedAt: undefined,
          };
        }
        state.error.delete = null;
      })
      .addCase(deletePublicationMetadataOnly.rejected, (state, action) => {
        state.loading.delete = false;
        state.error.delete = action.payload as string;
      });

    // Clear Errors
    builder.addCase(clearErrors.fulfilled, (state) => {
      state.error = {
        publications: null,
        stats: null,
        create: null,
        update: null,
        delete: null,
      };
    });
  },
});

export default publicationsSlice.reducer;
