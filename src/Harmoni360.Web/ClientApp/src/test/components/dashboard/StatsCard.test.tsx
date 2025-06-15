import { describe, it, expect } from 'vitest';
import { render, screen } from '../../utils/test-utils';
import { StatsCard } from '../../../components/dashboard/StatsCard';
import { faHeartbeat } from '@fortawesome/free-solid-svg-icons';

describe('StatsCard Component', () => {
  const defaultProps = {
    title: 'Total Health Records',
    value: '150',
    icon: faHeartbeat,
    color: 'success'
  };

  it('renders with basic props correctly', () => {
    render(<StatsCard {...defaultProps} />);
    
    expect(screen.getByText('Total Health Records')).toBeInTheDocument();
    expect(screen.getByText('150')).toBeInTheDocument();
  });

  it('displays trend information when provided', () => {
    render(
      <StatsCard 
        {...defaultProps} 
        trend={{ value: 12, isPositive: true }}
      />
    );
    
    expect(screen.getByText('+12')).toBeInTheDocument();
    expect(screen.getByText(/vs last month/i)).toBeInTheDocument();
  });

  it('shows negative trend correctly', () => {
    render(
      <StatsCard 
        {...defaultProps} 
        trend={{ value: 5, isPositive: false }}
      />
    );
    
    expect(screen.getByText('-5')).toBeInTheDocument();
  });

  it('displays subtitle when provided', () => {
    render(
      <StatsCard 
        {...defaultProps} 
        subtitle="Active records only"
      />
    );
    
    expect(screen.getByText('Active records only')).toBeInTheDocument();
  });

  it('applies correct color classes', () => {
    const { container } = render(<StatsCard {...defaultProps} color="danger" />);
    
    expect(container.querySelector('.border-danger')).toBeInTheDocument();
  });

  it('handles large numbers correctly', () => {
    render(<StatsCard {...defaultProps} value="1,234,567" />);
    
    expect(screen.getByText('1,234,567')).toBeInTheDocument();
  });

  it('renders FontAwesome icon correctly', () => {
    const { container } = render(<StatsCard {...defaultProps} />);
    
    expect(container.querySelector('svg')).toBeInTheDocument();
  });

  it('handles loading state', () => {
    render(<StatsCard {...defaultProps} value="..." isLoading={true} />);
    
    expect(screen.getByText('...')).toBeInTheDocument();
  });

  it('is accessible with proper ARIA labels', () => {
    render(<StatsCard {...defaultProps} />);
    
    const card = screen.getByRole('region');
    expect(card).toHaveAttribute('aria-label', expect.stringContaining('Total Health Records'));
  });
});