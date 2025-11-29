/**
 * SystemHealth Component Tests
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { SystemHealth } from '../components/SystemHealth';

describe('SystemHealth Component', () => {
  it('renders component title', () => {
    render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={62}
        diskUsage={38}
        status="healthy"
      />
    );

    expect(screen.getByText('System Health')).toBeInTheDocument();
  });

  it('displays healthy status', () => {
    render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={62}
        diskUsage={38}
        status="healthy"
      />
    );

    expect(screen.getByText('HEALTHY')).toBeInTheDocument();
  });

  it('displays warning status', () => {
    render(
      <SystemHealth
        cpuUsage={75}
        memoryUsage={82}
        diskUsage={68}
        status="warning"
      />
    );

    expect(screen.getByText('WARNING')).toBeInTheDocument();
  });

  it('displays critical status', () => {
    render(
      <SystemHealth
        cpuUsage={92}
        memoryUsage={95}
        diskUsage={88}
        status="critical"
      />
    );

    expect(screen.getByText('CRITICAL')).toBeInTheDocument();
  });

  it('displays CPU usage correctly', () => {
    render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={62}
        diskUsage={38}
        status="healthy"
      />
    );

    expect(screen.getByText('CPU Usage')).toBeInTheDocument();
    expect(screen.getByText('45%')).toBeInTheDocument();
  });

  it('displays Memory usage correctly', () => {
    render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={62}
        diskUsage={38}
        status="healthy"
      />
    );

    expect(screen.getByText('Memory Usage')).toBeInTheDocument();
    expect(screen.getByText('62%')).toBeInTheDocument();
  });

  it('displays Disk usage correctly', () => {
    render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={62}
        diskUsage={38}
        status="healthy"
      />
    );

    expect(screen.getByText('Disk Usage')).toBeInTheDocument();
    expect(screen.getByText('38%')).toBeInTheDocument();
  });

  it('shows progress bars', () => {
    const { container } = render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={62}
        diskUsage={38}
        status="healthy"
      />
    );

    const progressBars = container.querySelectorAll('[role="progressbar"]');
    expect(progressBars).toHaveLength(3);
  });

  it('applies correct color for low usage (< 60%)', () => {
    render(
      <SystemHealth
        cpuUsage={45}
        memoryUsage={55}
        diskUsage={38}
        status="healthy"
      />
    );

    // Low usage should use success color
    expect(screen.getByText('45%')).toBeInTheDocument();
    expect(screen.getByText('55%')).toBeInTheDocument();
    expect(screen.getByText('38%')).toBeInTheDocument();
  });

  it('applies correct color for medium usage (60-80%)', () => {
    render(
      <SystemHealth
        cpuUsage={65}
        memoryUsage={75}
        diskUsage={70}
        status="warning"
      />
    );

    // Medium usage should use warning color
    expect(screen.getByText('65%')).toBeInTheDocument();
    expect(screen.getByText('75%')).toBeInTheDocument();
    expect(screen.getByText('70%')).toBeInTheDocument();
  });

  it('applies correct color for high usage (>= 80%)', () => {
    render(
      <SystemHealth
        cpuUsage={85}
        memoryUsage={92}
        diskUsage={88}
        status="critical"
      />
    );

    // High usage should use error color
    expect(screen.getByText('85%')).toBeInTheDocument();
    expect(screen.getByText('92%')).toBeInTheDocument();
    expect(screen.getByText('88%')).toBeInTheDocument();
  });
});
