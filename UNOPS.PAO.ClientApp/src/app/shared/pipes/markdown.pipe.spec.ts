import { MarkdownPipe } from './markdown.pipe';

describe('MarkdownPipe', () => {
  let pipe: MarkdownPipe;

  beforeEach(() => {
    pipe = new MarkdownPipe();
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  // TODO: Add tests for markdown transformation
  // TODO: Add tests for edge cases (null, empty, etc.)
  // TODO: Add tests for various markdown syntax
});

