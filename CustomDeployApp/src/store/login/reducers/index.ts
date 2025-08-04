import {
  LOGIN_REQUEST,
  LOGIN_SUCCESS,
  LOGIN_FAILURE,
  AUTO_LOGIN_SUCCESS,
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
  isAutoLogin: false,
};

// Type guard para verificar se a action é do tipo LoginActionTypes
const isLoginAction = (action: AnyAction): action is LoginActionTypes => {
  return [LOGIN_REQUEST, LOGIN_SUCCESS, LOGIN_FAILURE, AUTO_LOGIN_SUCCESS, LOGOUT].includes(action.type as typeof LOGIN_REQUEST);
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
        isAutoLogin: false, // Login manual
      };

    case AUTO_LOGIN_SUCCESS:
      return {
        ...state,
        isLoading: false,
        error: null,
        isAuthenticated: true,
        user: action.payload,
        isAutoLogin: true, // Login automático
      };

    case LOGIN_FAILURE:
      return {
        ...state,
        isLoading: false,
        error: action.payload,
        isAuthenticated: false,
        user: null,
        isAutoLogin: false,
      };

    case LOGOUT:
      return {
        ...state,
        isLoading: false,
        error: null,
        isAuthenticated: false,
        user: null,
        isAutoLogin: false,
      };

    default:
      return state;
  }
};

export default loginReducer;