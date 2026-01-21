import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { TypewriterDirective } from './typewriter.directive';

describe('TypewriterDirective', () => {
  @Component({
    template: '<div appTypewriter></div>',
    imports: [TypewriterDirective]
  })
  class HostComponent {}

  let fixture: ComponentFixture<HostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
  });

  it('should create an instance', () => {
    const directive = fixture.debugElement.query(By.directive(TypewriterDirective));
    expect(directive).toBeTruthy();
  });

  // TODO: Add tests for typewriter effect
  // TODO: Add tests for text animation
  // TODO: Add tests for speed control
});

