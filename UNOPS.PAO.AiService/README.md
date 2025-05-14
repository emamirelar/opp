# Google Cloud Function - Hello World

This is a simple Hello World Google Cloud Function that returns a JSON response.

## Prerequisites

- Google Cloud SDK installed
- Python 3.7 or later
- A Google Cloud Project

## Local Testing

To test the function locally:

1. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Run the function locally using Functions Framework:
```bash
functions-framework --target hello_world --port 8080
```

3. Test the function:
```bash
curl http://localhost:8080
```

## Deployment

To deploy to Google Cloud:

```bash
gcloud functions deploy hello_world \
  --runtime python39 \
  --trigger-http \
  --allow-unauthenticated
```

After deployment, you'll receive a URL where your function is accessible. 