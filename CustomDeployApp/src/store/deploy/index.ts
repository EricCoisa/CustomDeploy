import { createSlice } from '@reduxjs/toolkit';
import type { DeployState } from './types';
import {
  executeDeploy,
  clearDeployState,
  clearError
} from './actions';

const initialState: DeployState = {
  isDeploying: false,
  lastDeployResult: null,
  error: null,
};

const deploySlice = createSlice({
  name: 'deploy',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    // Execute Deploy
    builder
      .addCase(executeDeploy.pending, (state) => {
        state.isDeploying = true;
        state.error = null;
      })
      .addCase(executeDeploy.fulfilled, (state, action) => {
        state.isDeploying = false;
        state.lastDeployResult = action.payload;
        state.error = null;
      })
      .addCase(executeDeploy.rejected, (state, action) => {
        state.isDeploying = false;
        state.error = action.payload as string;
      });

    // Clear Deploy State
    builder.addCase(clearDeployState.fulfilled, (state) => {
      state.lastDeployResult = null;
      state.error = null;
    });

    // Clear Error
    builder.addCase(clearError.fulfilled, (state) => {
      state.error = null;
    });
  },
});

export default deploySlice.reducer;
