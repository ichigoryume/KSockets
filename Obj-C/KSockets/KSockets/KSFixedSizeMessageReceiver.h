//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSIReceiver.h"

@interface KSFixedSizeMessageReceiver : NSObject <KSIReceiver>

- (id)initWithPayloadSize:(int)payloadSize;

@end
