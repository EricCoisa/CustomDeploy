import React from 'react';
import styled from 'styled-components';

export interface CardProps {
  title: string;
  value: string | number;
  icon?: React.ReactNode;
  color?: string;
  bgColor?: string;
  isLoading?: boolean;
  onClick?: () => void;
  className?: string;
}

const CardContainer = styled.div<{ 
  bgColor?: string; 
  hasClick?: boolean; 
}>`
  background: ${props => props.bgColor || '#ffffff'};
  border-radius: 0.75rem;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
  border: 1px solid #e5e7eb;
  transition: all 0.2s ease-in-out;
  cursor: ${props => props.hasClick ? 'pointer' : 'default'};

  &:hover {
    transform: ${props => props.hasClick ? 'translateY(-2px)' : 'none'};
    box-shadow: ${props => props.hasClick 
      ? '0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)' 
      : '0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)'
    };
  }
`;

const CardHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
`;

const CardTitle = styled.h3<{ color?: string }>`
  font-size: 0.875rem;
  font-weight: 500;
  color: ${props => props.color || '#6b7280'};
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const CardIcon = styled.div<{ color?: string }>`
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 0.5rem;
  background-color: ${props => props.color ? `${props.color}15` : '#f3f4f6'};
  color: ${props => props.color || '#6b7280'};
  font-size: 1.25rem;
`;

const CardValue = styled.div<{ color?: string }>`
  font-size: 2rem;
  font-weight: 700;
  color: ${props => props.color || '#111827'};
  line-height: 1;
`;

const LoadingSkeleton = styled.div`
  height: 2rem;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 0.25rem;

  @keyframes loading {
    0% {
      background-position: 200% 0;
    }
    100% {
      background-position: -200% 0;
    }
  }
`;

export const Card: React.FC<CardProps> = ({
  title,
  value,
  icon,
  color = '#3b82f6',
  bgColor,
  isLoading = false,
  onClick,
  className
}) => {
  return (
    <CardContainer 
      bgColor={bgColor} 
      hasClick={!!onClick}
      onClick={onClick}
      className={className}
    >
      <CardHeader>
        <CardTitle color={color}>{title}</CardTitle>
        {icon && (
          <CardIcon color={color}>
            {icon}
          </CardIcon>
        )}
      </CardHeader>
      
      {isLoading ? (
        <LoadingSkeleton />
      ) : (
        <CardValue color={color}>
          {value}
        </CardValue>
      )}
    </CardContainer>
  );
};
