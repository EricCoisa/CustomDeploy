import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Button } from '../Button';
import { FormInput } from './styles';

interface SortableBuildCommandProps {
  id: string;
  command: {
    comando: string;
    terminalId: string;
  };
  index: number;
  isDeploying: boolean;
  onCommandChange: (index: number, field: 'comando' | 'terminalId', value: string) => void;
  onRemove: (index: number) => void;
}

export function SortableBuildCommand({
  id,
  command,
  index,
  isDeploying,
  onCommandChange,
  onRemove,
}: SortableBuildCommandProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    isDragging,
  } = useSortable({ id });

  const style: React.CSSProperties = {
    position: 'relative',
    transform: CSS.Transform.toString(transform),
    transition: isDragging 
      ? 'none' // Durante o arraste, não aplicamos transição
      : 'transform 200ms cubic-bezier(0.2, 0, 0, 1)', // Ao soltar, transição suave
    opacity: isDragging ? 0.8 : 1,
    backgroundColor: '#ffffff',
    padding: '8px',
    borderRadius: '4px',
    marginBottom: '8px',
    cursor: isDragging ? 'grabbing' : 'grab',
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    border: '1px solid #e0e0e0',
    boxShadow: isDragging 
      ? '0 8px 16px rgba(0,0,0,0.1)' 
      : 'none',
    touchAction: 'none',
    zIndex: isDragging ? 999 : 1,
    userSelect: 'none',
    willChange: 'transform', // Otimiza a performance das transformações
  };

  return (
    <div ref={setNodeRef} style={style}>
      <span 
        style={{ 
          color: isDragging ? '#0066cc' : '#666', 
          marginRight: '8px',
          cursor: 'grab',
          userSelect: 'none',
          display: 'flex',
          alignItems: 'center'
        }}
        {...attributes}
        {...listeners}
      >☰</span>
      <FormInput
        type="text"
        placeholder="Comando (ex: npm install && npm run build)"
        value={command.comando}
        onChange={(e) => onCommandChange(index, 'comando', e.target.value)}
        disabled={isDeploying}
        style={{ flex: 2 }}
      />
      <FormInput
        type="text"
        placeholder="Terminal ID (ex: 1)"
        value={command.terminalId}
        onChange={(e) => onCommandChange(index, 'terminalId', e.target.value)}
        disabled={isDeploying}
        style={{ flex: 1 }}
      />
      <Button
        type="button"
        size="small"
        variant="secondary"
        onClick={() => onRemove(index)}
        disabled={isDeploying}
      >
        ✕
      </Button>
    </div>
  );
}
