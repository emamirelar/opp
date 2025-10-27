import { Component, Input, OnInit, ViewChild, ElementRef, AfterViewInit, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

@Component({
  selector: 'app-chart-js',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chart-js.component.html',
  styleUrls: ['./chart-js.component.scss']
})
export class ChartJsComponent implements OnInit, AfterViewInit {
  @Input() chartType: string = 'pie';
  @Input() chartConfig: any;
  @Input() chartData: any;

  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;

  private chart?: Chart;
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);

  ngOnInit() {
    console.log('🎯 ChartJS Component - Chart Type:', this.chartType);
    console.log('🎯 ChartJS Component - Chart Config:', this.chartConfig);
    console.log('🎯 ChartJS Component - Chart Data:', this.chartData);

    // Register Chart.js components
    if (this.isBrowser) {
      Chart.register(...registerables);
    }
  }

  ngAfterViewInit() {
    if (this.isBrowser && this.chartCanvas) {
      setTimeout(() => {
        this.createChart();
      }, 100);
    }
  }

  ngOnDestroy() {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  private createChart() {
    if (!this.chartCanvas?.nativeElement) {
      console.error('🎯 Chart canvas not available');
      return;
    }

    try {
      const ctx = this.chartCanvas.nativeElement.getContext('2d');
      if (!ctx) {
        console.error('🎯 Cannot get 2D context');
        return;
      }

      // Prepare chart configuration
      const config = this.prepareChartConfig();
      
      console.log('🎯 Creating chart with config:', config);

      // Create chart
      this.chart = new Chart(ctx, config);
      
      console.log('🎯 Chart created successfully');
    } catch (error) {
      console.error('🎯 Failed to create chart:', error);
    }
  }

  private prepareChartConfig(): ChartConfiguration {
    // Use provided config or create default
    const data = this.chartConfig?.data || this.chartData || {
      labels: ['No Data'],
      datasets: [{ data: [1], label: 'No Data' }]
    };

    const options = this.chartConfig?.options || this.getDefaultOptions();

    const config: ChartConfiguration = {
      type: this.chartType as ChartType,
      data: data,
      options: options
    };

    // Apply chart-specific configurations
    this.applyChartSpecificConfig(config);

    return config;
  }

  private getDefaultOptions(): any {
    const baseOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top' as const,
        },
        title: {
          display: !!this.chartConfig?.title,
          text: this.chartConfig?.title || ''
        }
      }
    };

    // Chart-specific options
    switch (this.chartType) {
      case 'pie':
      case 'doughnut':
        return {
          ...baseOptions,
          plugins: {
            ...baseOptions.plugins,
            tooltip: {
              callbacks: {
                label: (context: any) => {
                  const label = context.label || '';
                  const value = context.parsed || 0;
                  const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
                  const percentage = ((value / total) * 100).toFixed(1);
                  return `${label}: ${value} (${percentage}%)`;
                }
              }
            }
          }
        };

      case 'bar':
      case 'line':
        return {
          ...baseOptions,
          scales: {
            y: {
              beginAtZero: true
            }
          }
        };

      case 'radar':
        return {
          ...baseOptions,
          scales: {
            r: {
              beginAtZero: true
            }
          }
        };

      default:
        return baseOptions;
    }
  }

  private applyChartSpecificConfig(config: ChartConfiguration) {
    // Add default colors if not provided
    if (config.data?.datasets) {
      config.data.datasets.forEach((dataset: any, index: number) => {
        if (!dataset.backgroundColor) {
          dataset.backgroundColor = this.getDefaultColors(config.data?.labels?.length || 1);
        }
        
        // Chart-specific styling
        switch (this.chartType) {
          case 'line':
            dataset.borderColor = dataset.backgroundColor;
            dataset.backgroundColor = dataset.backgroundColor?.map((color: string) => 
              color.replace('1)', '0.2)')
            );
            dataset.fill = false;
            break;
            
          case 'bar':
            dataset.borderWidth = 1;
            dataset.borderColor = dataset.backgroundColor?.map((color: string) => 
              color.replace('0.8)', '1)')
            );
            break;
        }
      });
    }
  }

  private getDefaultColors(count: number): string[] {
    const colors = [
      'rgba(255, 99, 132, 0.8)',   // Red
      'rgba(54, 162, 235, 0.8)',   // Blue
      'rgba(255, 205, 86, 0.8)',   // Yellow
      'rgba(75, 192, 192, 0.8)',   // Green
      'rgba(153, 102, 255, 0.8)',  // Purple
      'rgba(255, 159, 64, 0.8)',   // Orange
      'rgba(199, 199, 199, 0.8)',  // Grey
      'rgba(83, 102, 255, 0.8)',   // Indigo
      'rgba(255, 99, 255, 0.8)',   // Pink
      'rgba(99, 255, 132, 0.8)'    // Light Green
    ];

    // Repeat colors if we need more
    const result = [];
    for (let i = 0; i < count; i++) {
      result.push(colors[i % colors.length]);
    }
    return result;
  }
}
