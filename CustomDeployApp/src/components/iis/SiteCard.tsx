import React, { useState } from 'react';
import { Button } from '../../components';
import type { IISSite, IISApplication } from '../../store/iis/types';
import styled from 'styled-components';

const SiteCard = styled.div`
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  background: white;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  overflow: hidden;
`;

const SiteHeader = styled.div`
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  
  &:hover {
    background-color: #f9fafb;
  }
  
  @media (max-width: 768px) {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
`;

const SiteInfo = styled.div`
  flex: 1;
`;

const SiteName = styled.h3`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.1rem;
`;

const SiteDetails = styled.div`
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  color: #6b7280;
  flex-wrap: wrap;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 0.5rem;
  }
`;

const StatusBadge = styled.span<{ status: string }>`
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
  background-color: ${props => 
    props.status === 'Started' ? '#28a745' : 
    props.status === 'Stopped' ? '#dc3545' : '#6c757d'};
  color: white;
`;

const ApplicationsSection = styled.div`
  padding: 1rem;
  background-color: #f9fafb;
`;

const ApplicationList = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
`;

const ApplicationItem = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: white;
  border-radius: 0.375rem;
  border: 1px solid #e5e7eb;
  
  @media (max-width: 768px) {
    flex-direction: column;
    align-items: stretch;
    gap: 0.75rem;
  }
`;

const ApplicationInfo = styled.div`
  flex: 1;
`;

const ApplicationPath = styled.span`
  font-weight: bold;
  color: #1f2937;
`;

const ApplicationDetails = styled.div`
  font-size: 0.9rem;
  color: #6b7280;
  margin-top: 0.25rem;
`;

const ActionButtons = styled.div`
  display: flex;
  gap: 0.5rem;
  
  @media (max-width: 768px) {
    justify-content: center;
    flex-wrap: wrap;
  }
`;

const ExpandIcon = styled.span<{ expanded: boolean }>`
  transition: transform 0.2s;
  transform: ${props => props.expanded ? 'rotate(90deg)' : 'rotate(0deg)'};
`;

interface SiteCardComponentProps {
  site: IISSite;
  onEditSite: (site: IISSite) => void;
  onDeleteSite: (siteName: string) => void;
  onAddApplication: (siteName: string) => void;
  onEditApplication: (siteName: string, appPath: string, application: IISApplication) => void;
  onDeleteApplication: (siteName: string, appPath: string) => void;
  onStartSite: (siteName: string) => void;
  onStopSite: (siteName: string) => void;
  onStartApplication: (siteName: string, appPath: string) => void;
  onStopApplication: (siteName: string, appPath: string) => void;
}

export const SiteCardComponent: React.FC<SiteCardComponentProps> = ({
  site,
  onEditSite,
  onDeleteSite,
  onAddApplication,
  onEditApplication,
  onDeleteApplication,
  onStartSite,
  onStopSite,
  onStartApplication,
  onStopApplication
}) => {
  const [expanded, setExpanded] = useState(false);

  const isRunning = site.state === 'Started' || site.state === 'Running';

  return (
    <SiteCard>
      <SiteHeader onClick={() => setExpanded(!expanded)}>
        <SiteInfo>
          <SiteName>
            <ExpandIcon expanded={expanded}>▶</ExpandIcon> {site.name}
          </SiteName>
          <SiteDetails>
            <div>
              <strong>Status:</strong> <StatusBadge status={site.state}>{site.state}</StatusBadge>
            </div>
            <div><strong>Bindings:</strong> {site.bindings?.map(b => `${b.protocol}://${b.ipAddress}:${b.port}`).join(', ') || 'N/A'}</div>
            <div><strong>App Pool:</strong> {site.appPoolName}</div>
          </SiteDetails>
        </SiteInfo>
        <ActionButtons>
          {isRunning ? (
            <Button 
              size="small" 
              variant="secondary"
              onClick={(e) => {
                e.stopPropagation();
                onStopSite(site.name);
              }}
            >
              ⏹ Parar
            </Button>
          ) : (
            <Button 
              size="small" 
              variant="primary"
              onClick={(e) => {
                e.stopPropagation();
                onStartSite(site.name);
              }}
            >
              ▶ Iniciar
            </Button>
          )}
          <Button 
            size="small" 
            onClick={(e) => {
              e.stopPropagation();
              onAddApplication(site.name);
            }}
          >
            + Aplicação
          </Button>
          <Button 
            size="small" 
            variant="secondary"
            onClick={(e) => {
              e.stopPropagation();
              onEditSite(site);
            }}
          >
            Editar
          </Button>
          <Button 
            size="small" 
            variant="danger"
            onClick={(e) => {
              e.stopPropagation();
              onDeleteSite(site.name);
            }}
          >
            Excluir
          </Button>
        </ActionButtons>
      </SiteHeader>

      {expanded && (
        <ApplicationsSection>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <h4 style={{ margin: 0, color: '#333' }}>
              Aplicações ({site.applications?.length || 0})
            </h4>
          </div>

          {site.applications && site.applications.length > 0 ? (
            <ApplicationList>
              {site.applications.map((app, index) => (
                <ApplicationItem key={`${app.name}-${index}`}>
                  <ApplicationInfo>
                    <ApplicationPath>{app.name}</ApplicationPath>
                    <ApplicationDetails>
                      <div><strong>Caminho Físico:</strong> {app.physicalPath}</div>
                      <div><strong>App Pool:</strong> {app.applicationPool}</div>
                      <div><strong>Protocolos:</strong> {app.enabledProtocols}</div>
                    </ApplicationDetails>
                  </ApplicationInfo>
                  <ActionButtons>
                    <Button 
                      size="small" 
                      variant="primary"
                      onClick={() => onStartApplication(site.name, app.name)}
                    >
                      ▶ Iniciar
                    </Button>
                    <Button 
                      size="small" 
                      variant="secondary"
                      onClick={() => onStopApplication(site.name, app.name)}
                    >
                      ⏹ Parar
                    </Button>
                    <Button 
                      size="small" 
                      variant="secondary"
                      onClick={() => onEditApplication(site.name, app.name, app)}
                    >
                      Editar
                    </Button>
                    <Button 
                      size="small" 
                      variant="danger"
                      onClick={() => onDeleteApplication(site.name, app.name)}
                    >
                      Excluir
                    </Button>
                  </ActionButtons>
                </ApplicationItem>
              ))}
            </ApplicationList>
          ) : (
            <div style={{ 
              textAlign: 'center', 
              padding: '2rem', 
              color: '#666',
              fontStyle: 'italic'
            }}>
              Nenhuma aplicação encontrada neste site
            </div>
          )}
        </ApplicationsSection>
      )}
    </SiteCard>
  );
};
