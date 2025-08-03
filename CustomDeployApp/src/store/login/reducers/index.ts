import {
  LOGIN_REQUEST,
  LOGIN_SUCCESS,
  LOGIN_FAILURE,
  LOGOUT,
  type LoginState,
  type LoginActionTypes,
} from '../types';
import { type AnyAction } from 'redux';

// Estado inicial
const initialState: LoginState = {
  isLoading: false,
  error: null,
  isAuthenticated: false,
  user: null,
};

// Type guard para verificar se a action é do tipo LoginActionTypes
const isLoginAction = (action: AnyAction): action is LoginActionTypes => {
  return [LOGIN_REQUEST, LOGIN_SUCCESS, LOGIN_FAILURE, LOGOUT].includes(action.type as typeof LOGIN_REQUEST);
};

// Reducer
const loginReducer = (
  state = initialState,
  action: AnyAction
): LoginState => {
  if (!isLoginAction(action)) {
    return state;
  }

  switch (action.type) {
    case LOGIN_REQUEST:
      return {
        ...state,
        isLoading: true,
        error: null,
      };

    case LOGIN_SUCCESS:
      return {
        ...state,
        isLoading: false,
        error: null,
        isAuthenticated: true,
        user: action.payload,
      };

    case LOGIN_FAILURE:
      return {
        ...state,
        isLoading: false,
        error: action.payload,
        isAuthenticated: false,
        user: null,
      };

    case LOGOUT:
      return {
        ...state,
        isLoading: false,
        error: null,
        isAuthenticated: false,
        user: null,
      };

    default:
      return state;
  }
};

export default loginReducer;