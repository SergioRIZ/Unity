mergeInto(LibraryManager.library, {
  IsMobileUserAgent: function() {
    // Lista de patrones en User Agent que indican un dispositivo móvil
    var mobilePatterns = [
      'Android', 'webOS', 'iPhone', 'iPad', 'iPod', 'BlackBerry', 
      'Windows Phone', 'Mobile', 'mobile', 'CriOS', 'FxiOS'
    ];
    
    var userAgent = navigator.userAgent;
    
    for (var i = 0; i < mobilePatterns.length; i++) {
      if (userAgent.indexOf(mobilePatterns[i]) !== -1) {
        // Es un dispositivo móvil
        return true;
      }
    }
    
    // Detección adicional mediante media queries
    var isMobile = false;
    if (window.matchMedia) {
      isMobile = window.matchMedia('(max-width: 768px)').matches || 
                window.matchMedia('(pointer: coarse)').matches;
    }
    
    return isMobile;
  }
});