import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EntityArtifactManagerComponent } from './entity-artifact-manager.component';

describe('EntityArtifactManagerComponent', () => {
  let component: EntityArtifactManagerComponent;
  let fixture: ComponentFixture<EntityArtifactManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EntityArtifactManagerComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EntityArtifactManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

