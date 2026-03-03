// Karma CI configuration
// Excludes spec files with known TestBed configuration issues (Angular 19 migration debt).
// These tests need incremental fixes for: standalone-in-declarations, missing providers, etc.
// Tracked in: QA Tests/Defect List for QA.md

const baseConfig = require('./karma.conf.js');

module.exports = function (config) {
  baseConfig(config);

  config.set({
    browsers: ['ChromeHeadlessNoSandbox'],
    customLaunchers: {
      ChromeHeadlessNoSandbox: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox', '--disable-gpu', '--disable-dev-shm-usage']
      }
    },
    singleRun: true,
    restartOnFileChange: false,
    reporters: ['progress'],
    browserNoActivityTimeout: 120000,
    browserDisconnectTimeout: 60000,
    browserDisconnectTolerance: 3,
    captureTimeout: 120000,
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage/UNOPS.PAO.ClientApp'),
      subdir: '.',
      reporters: [
        { type: 'text-summary' },
        { type: 'lcov' }
      ]
    },
  });
};
