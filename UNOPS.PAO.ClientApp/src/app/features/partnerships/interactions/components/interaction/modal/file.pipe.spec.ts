import { FilePipe } from './file.pipe';

describe('FilePipe', () => {
  let pipe: FilePipe;

  beforeEach(() => {
    pipe = new FilePipe();
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  // TODO: Add tests for file transformation
  // TODO: Add tests for file size formatting
  // TODO: Add tests for file name handling
});

