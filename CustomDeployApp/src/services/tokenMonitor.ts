import { authService } from './authService';

class TokenMonitorService {
  private intervalId: number | null = null;
  private onTokenExpired?: () => void;
  
  // Iniciar monitoramento de token
  startMonitoring(onTokenExpired: () => void) {
    this.onTokenExpired = onTokenExpired;
    
    // Verificar a cada 30 segundos
    this.intervalId = setInterval(() => {
      if (!authService.isAuthenticated()) {
        console.log('🚨 Token expirado detectado, fazendo logout automático');
        this.onTokenExpired?.();
        this.stopMonitoring();
      } else if (authService.isTokenExpiring()) {
        console.log('⚠️ Token próximo do vencimento');
        // Aqui poderia implementar renovação automática se o backend suportasse
      }
    }, 30000); // 30 segundos
    
    console.log('🔍 Monitoramento de token iniciado');
  }
  
  // Parar monitoramento
  stopMonitoring() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
      console.log('🛑 Monitoramento de token parado');
    }
  }
  
  // Verificar se está monitorando
  isMonitoring(): boolean {
    return this.intervalId !== null;
  }
}

export const tokenMonitor = new TokenMonitorService();
