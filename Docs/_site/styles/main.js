(function($) {
  $.fn.imageComparison = function() {
    return this.each(function() {
      var $this = $(this);
      var left = $this.find('.image-cmp-l');
      var right = $this.find('.image-cmp-r');
      var leftLabel = left.find('img').attr('alt');
      var rightLabel = right.find('img').attr('alt');
      var leftTab = $('<div class="image-cmp-tab">' + leftLabel + '</div>');
      var rightTab = $('<div class="image-cmp-tab">' + rightLabel + '</div>');
      var bothTab = $('<div class="image-cmp-tab">Both</div>');
      function setActiveTab(tab) {
        leftTab.removeClass('image-cmp-tab-active');
        rightTab.removeClass('image-cmp-tab-active');
        bothTab.removeClass('image-cmp-tab-active');
        tab.addClass('image-cmp-tab-active');
      };
      function activateLeft() {
        left.show();
        right.hide();
        setActiveTab(leftTab);
      };
      function activateRight() {
        left.hide();
        right.show();
        setActiveTab(rightTab);
      };
      function activateBoth() {
        left.show();
        right.show();
        setActiveTab(bothTab);
      };
      leftTab.click(activateLeft);
      rightTab.click(activateRight);
      bothTab.click(activateBoth);
      var tabs = $('<div class="image-cmp-tabs"/>').append(leftTab).append(rightTab).append(bothTab);
      $this.wrap('<div/>').before(tabs);
      activateLeft();
    });
  };
}(jQuery));

(function($) {
  $(function() {
    $('figure.image-cmp').imageComparison();
  });
}(jQuery));
