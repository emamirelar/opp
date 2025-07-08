/*
  Navigating pages in the add-on
*/
function prevPage(e) {
  const props = PropertiesService.getUserProperties();
  let currentPage = parseInt(props.getProperty('currentPage')) || 1;
  currentPage = Math.max(1, currentPage - 1);
  props.setProperty('currentPage', currentPage.toString());

  const originalE = JSON.parse(props.getProperty('originalE'));
  return createCard(originalE);
}

function nextPage(e) {
  const props = PropertiesService.getUserProperties();
  let currentPage = parseInt(props.getProperty('currentPage')) || 1;
  currentPage += 1;
  props.setProperty('currentPage', currentPage.toString());

  const originalE = JSON.parse(props.getProperty('originalE'));
  return createCard(originalE);
}